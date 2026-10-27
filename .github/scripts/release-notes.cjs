/* global module, process */ // runs in Node.js (actions/github-script), not in the browser
// Builds the release notes for the next release and writes them to a draft GitHub Release.
//
// PRs merged into main since the last release -> the issue behind each PR -> its title.
// One line per issue, however many PRs it had.
// Left out: PRs with "geen-release-note" in the description, issues with the "Leave this issue out of
// the release notes" box ticked, and dependabot PRs (summarised as one line).
// PRs without a linked issue are listed under "Before publishing" for the release manager.
//
// checkPr runs on every PR to main and fails it when the release notes couldn't place it: no linked issue
// and no "geen-release-note". It uses the same rules as the release notes themselves.
//
// Called from .github/workflows/release-notes.yml via actions/github-script.

const DRAFT_NAME = "Next release";
const DRAFT_TAG = "next-release";
const RELEASE_BRANCH = "main";
const SKIP_MARKER = "geen-release-note";
const DEPENDABOT = "dependabot[bot]";
// the version isn't known until the release is published
const VERSION_PLACEHOLDER = "X.Y.Z";

async function draft({ github, context, core }) {
  const { owner, repo } = context.repo;
  const dryRun = process.env.DRY_RUN === "true";

  const from = process.env.FROM_REF || (await getLatestReleaseTag());
  const to = process.env.TO_REF || context.sha;
  core.info(`Release notes for ${from}..${to}${dryRun ? " (dry run)" : ""}`);

  // 1. PRs merged into main in this release, found through the commits
  const prs = new Map();
  for (const sha of await getCommits(from, to)) {
    const { data } = await github.request("GET /repos/{owner}/{repo}/commits/{sha}/pulls", { owner, repo, sha });
    for (const pr of data) if (pr.merged_at && pr.base.ref === RELEASE_BRANCH) prs.set(pr.number, pr);
  }

  // 2. The issue behind each PR
  const attention = [];
  const issueNumbers = new Set();
  let hasDependencyUpdates = false;

  for (const pr of prs.values()) {
    if (isSkippedPr(pr)) continue;
    // dependabot PRs
    if (pr.user.login === DEPENDABOT) {
      hasDependencyUpdates = true;
      continue;
    }

    const numbers = await getIssueNumbersForPr(github, context.repo, pr);
    if (numbers.length === 0) attention.push(`PR #${pr.number} ${pr.title}: no linked issue`);
    numbers.forEach((n) => issueNumbers.add(n));
  }

  // 3. One line per issue
  const items = [];
  for (const number of issueNumbers) {
    const issue = await getIssue(number);
    if (!issue || issue.skip) continue; // not an issue (e.g. "#123" pointed to a PR), or marked "geen release note"
    items.push(`- ${escapeHtml(issue.title)} ([#${issue.number}](${issue.url}))`);
  }
  if (hasDependencyUpdates) items.push("- Dependency updates");

  // 4. Write the draft release
  const body = composeBody({ items, attention });

  await core.summary.addRaw(body).write();
  if (dryRun) {
    core.info(body);
    return;
  }
  await upsertDraftRelease(body);

  // --- helpers -------------------------------------------------------------------------------

  async function getLatestReleaseTag() {
    const { data } = await github.request("GET /repos/{owner}/{repo}/releases/latest", { owner, repo });
    return data.tag_name;
  }

  async function getCommits(base, head) {
    const shas = [];
    for (let page = 1; ; page++) {
      const { data } = await github.request("GET /repos/{owner}/{repo}/compare/{basehead}", {
        owner,
        repo,
        basehead: `${base}...${head}`,
        per_page: 100,
        page,
      });
      shas.push(...data.commits.map((c) => c.sha));
      if (data.commits.length < 100 || shas.length >= data.total_commits) return shas;
    }
  }

  async function getIssue(number) {
    let node;
    try {
      const { repository } = await github.graphql(
        `query($owner: String!, $repo: String!, $number: Int!) {
          repository(owner: $owner, name: $repo) {
            issueOrPullRequest(number: $number) {
              ... on Issue { number title body url }
            }
          }
        }`,
        { owner, repo, number },
      );
      node = repository.issueOrPullRequest;
    } catch {
      return null;
    }
    if (node?.title === undefined) return null;

    return {
      number: node.number,
      title: node.title,
      url: node.url,
      skip: hasNoReleaseNoteTicked(node.body),
    };
  }

  async function upsertDraftRelease(body) {
    const { data: releases } = await github.request("GET /repos/{owner}/{repo}/releases", { owner, repo, per_page: 30 });
    // matched on tag or name, so a renamed draft doesn't lead to a second one
    const draft = releases.find((r) => r.draft && (r.tag_name === DRAFT_TAG || r.name === DRAFT_NAME));

    if (draft) {
      await github.request("PATCH /repos/{owner}/{repo}/releases/{release_id}", { owner, repo, release_id: draft.id, body });
      core.info(`Updated draft release ${draft.html_url}`);
    } else {
      const { data } = await github.request("POST /repos/{owner}/{repo}/releases", {
        owner,
        repo,
        name: DRAFT_NAME,
        tag_name: DRAFT_TAG,
        target_commitish: RELEASE_BRANCH,
        draft: true,
        body,
      });
      core.info(`Created draft release ${data.html_url}`);
    }
  }
}

// The check on a PR to main: can the release notes place this PR?
async function checkPr({ github, context, core }) {
  const pr = context.payload.pull_request;
  if (pr.user.login === DEPENDABOT) return; // summarised as "Dependency updates"
  if (isSkippedPr(pr)) return;
  if ((await getIssueNumbersForPr(github, context.repo, pr)).length > 0) return;

  core.setFailed(
    'Add "Closes #<issue number>" to the PR description, or "geen-release-note" if this change needs no release note.',
  );
}

module.exports = { draft, checkPr };

// "Closes #123" in the description (GitHub's own link, also set through the Development sidebar),
// plus any other "#123" in the title or description
async function getIssueNumbersForPr(github, { owner, repo }, pr) {
  const { repository } = await github.graphql(
    `query($owner: String!, $repo: String!, $number: Int!) {
      repository(owner: $owner, name: $repo) {
        pullRequest(number: $number) {
          closingIssuesReferences(first: 20) { nodes { number repository { nameWithOwner } } }
        }
      }
    }`,
    { owner, repo, number: pr.number },
  );
  // only this repository's issues count
  const numbers = new Set(
    repository.pullRequest.closingIssuesReferences.nodes
      .filter((n) => n.repository.nameWithOwner.toLowerCase() === `${owner}/${repo}`.toLowerCase())
      .map((n) => n.number),
  );

  const text = `${pr.title}\n${stripComments(pr.body)}`;
  for (const m of text.matchAll(/(?:^|[^\w/])#(\d+)\b/g)) numbers.add(Number(m[1]));

  numbers.delete(pr.number);
  return [...numbers];
}

// "geen-release-note" in the PR description
function isSkippedPr(pr) {
  return stripComments(pr.body).toLowerCase().includes(SKIP_MARKER);
}

// A ticked box: "- [X] Leave this issue out of the release notes" (older issues: "Geen release note")
function hasNoReleaseNoteTicked(body) {
  return /^\s*[-*]\s*\[[xX]\]\s*(?:leave this issue out of the release notes|geen[- ]release[- ]note)\b/im.test(body ?? "");
}

// repeated until nothing changes, so nested input like "<!<!-- -->-- ..." can't leave a "<!--" behind
function stripComments(text) {
  let result = text ?? "";
  let previous;
  do {
    previous = result;
    result = result.replace(/<!--[\s\S]*?-->/g, "");
  } while (result !== previous);
  return result;
}

// titles come from issues anyone can edit; "<" or ">" in a title must not break the release body
function escapeHtml(text) {
  return text.replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

function composeBody({ items, attention }) {
  const lines = [
    "### Helm chart",
    `\`helm install my-ita-release oci://ghcr.io/interne-taak-afhandeling/internetaakafhandeling --version ${VERSION_PLACEHOLDER}\``,
    "",
    "### Documentation",
    `https://ita.readthedocs.io/nl/v${VERSION_PLACEHOLDER}/`,
    "",
  ];

  lines.push("### What's Changed", ...(items.length > 0 ? items : ["- No changes"]), "");

  lines.push(
    "<!-- Remove this section before publishing -->",
    "### Before publishing",
    `- Replace \`${VERSION_PLACEHOLDER}\` (2×) with the version, e.g. \`3.4.0\``,
    `- Set the tag to \`v${VERSION_PLACEHOLDER}\` (not \`${DRAFT_TAG}\`), otherwise no build runs`,
    ...attention.map((a) => `- ${a}`),
    "",
  );
  return lines.join("\n");
}
