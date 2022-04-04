# LBH ASC Brokerage API

The ASB Brokerage API is the backend for the Adult Social Care Broker tool.

## Setup

We recommend using [Docker Desktop][1] to get setup quickly. Once installed, set the environment variable `LBHPACKAGESTOKEN` to a Github access token, with the  `read:packages` scope enabled.

Run `make test` to test, and `make serve` to build. The API should then serve from [:5001][3].

[1]: https://www.docker.com/products/docker-desktop
[2]: https://github.com/settings/tokens/new?scopes=read:packages&description=LBH+Packages+Token
[3]: http://localhost:5001