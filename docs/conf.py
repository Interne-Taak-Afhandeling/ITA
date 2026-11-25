# Configuration file for the Sphinx documentation builder.
#
# This file only contains a selection of the most common options. For a full
# list see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

# -- Path setup --------------------------------------------------------------

# If extensions (or modules to document with autodoc) are in another directory,
# add these directories to sys.path here. If the directory is relative to the
# documentation root, use os.path.abspath to make it absolute, like shown here.
#
import os
# import sys
# sys.path.insert(0, os.path.abspath('.'))


# -- Project information -----------------------------------------------------

project = "ITA - Contactverzoeken afhandelen"
copyright = "2025, ICATT.menselijk digitaal"
author = "ICATT.menselijk digitaal"


# -- General configuration ---------------------------------------------------
# -- General configuration

extensions = [
    "sphinx.ext.duration",
    "sphinx.ext.doctest",
    "sphinx.ext.autodoc",
    "sphinx.ext.autosummary",
    "sphinx.ext.intersphinx",
    "sphinx_rtd_theme",
    "myst_parser",
]

# -- Link naar de README van de helm chart ------------------------------------
# De configuratie van ITA staat in charts/internetaakafhandeling/README.md. Read the Docs
# zet READTHEDOCS_GIT_IDENTIFIER op de branch of tag van de versie die gebouwd wordt
# (bijv. "main" voor latest en "v3.4.0" voor stable/tags). Zo linkt elke versie van
# deze documentatie naar de README die bij dezelfde versie van ITA hoort.
# Lokaal (zonder Read the Docs) en bij PR-previews ("external" versies, waar de
# identifier het PR-nummer is in plaats van een branch of tag) wordt naar main gelinkt.
if os.environ.get("READTHEDOCS_VERSION_TYPE") == "external":
    _git_ref = "main"
else:
    _git_ref = os.environ.get("READTHEDOCS_GIT_IDENTIFIER") or "main"
_helm_readme_url = (
    "https://github.com/Interne-Taak-Afhandeling/ITA/blob/"
    f"{_git_ref}/charts/internetaakafhandeling/README.md"
)

myst_enable_extensions = ["substitution"]
myst_substitutions = {
    "helm_readme_version": _git_ref,
    "helm_readme_url": _helm_readme_url,
    "helm_readme_link": f"[README van de helm chart]({_helm_readme_url})",
}

intersphinx_mapping = {
    "rtd": ("https://docs.readthedocs.io/en/stable/", None),
    "python": ("https://docs.python.org/3/", None),
    "sphinx": ("https://www.sphinx-doc.org/en/master/", None),
}
intersphinx_disabled_domains = ["std"]

templates_path = ["_templates"]

# -- Options for EPUB output
epub_show_urls = "footnote"

# List of patterns, relative to source directory, that match files and
# directories to ignore when looking for source files.
# This pattern also affects html_static_path and html_extra_path.
exclude_patterns = ["_build", "Thumbs.db", ".DS_Store"]

# -- Options for HTML output -------------------------------------------------

# The theme to use for HTML and HTML Help pages.  See the documentation for
# a list of builtin themes.
#
html_theme = "sphinx_rtd_theme"

# Add any paths that contain custom static files (such as style sheets) here,
# relative to this directory. They are copied after the builtin static files,
# so a file named "default.css" will overwrite the builtin "default.css".
html_static_path = ["_static"]

html_style = 'css/custom.css'
