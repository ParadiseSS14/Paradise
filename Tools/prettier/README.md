## YAML formatting

This tool keeps YAML formatting consistent and reduces maintainer workload. It uses a pre-commit hook to check staged YAML files automatically.

### Requirements

Install [Node.js](https://nodejs.org/) before using the formatter.

### Install dependencies

```bash
npm ci --prefix Tools/prettier
```

This installs the exact Prettier version used by the GitHub workflow. Different versions may format files differently and cause CI checks to fail. Always install dependencies from the committed lock file.

### Check formatting

```bash
npm --prefix Tools/prettier run prettier:check -- ../../Resources/Prototypes
```

### Apply formatting

```bash
npm --prefix Tools/prettier run prettier:write -- ../../Resources/Prototypes
```
