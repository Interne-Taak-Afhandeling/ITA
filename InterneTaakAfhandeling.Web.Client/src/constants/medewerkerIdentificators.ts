export const KnownMedewerkerIdentificators = {
  codeObjecttype: "mdw",
  emailFromEntraId: {
    codeRegister: "msei",
    codeSoortObjectId: "email"
  },
  emailHandmatig: {
    codeRegister: "handmatig",
    codeSoortObjectId: "email"
  },
  objectRegisterId: {
    codeRegister: "obj",
    codeSoortObjectId: "idf"
  }
} as const;

type MedewerkerIdentificator = (typeof KnownMedewerkerIdentificators)[Exclude<
  keyof typeof KnownMedewerkerIdentificators,
  "codeObjecttype"
>];

export const matchesIdentificator = (
  actoridentificator:
    | { codeRegister?: string; codeSoortObjectId?: string; objectId?: string }
    | undefined,
  known: MedewerkerIdentificator,
  objectId: string,
  options?: { caseInsensitive?: boolean }
) =>
  actoridentificator?.codeRegister === known.codeRegister &&
  actoridentificator?.codeSoortObjectId === known.codeSoortObjectId &&
  (options?.caseInsensitive
    ? actoridentificator?.objectId?.toLowerCase() === objectId.toLowerCase()
    : actoridentificator?.objectId === objectId);
