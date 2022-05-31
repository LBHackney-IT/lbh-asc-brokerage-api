# LBH ASC Brokerage API

The ASB Brokerage API is the backend for the Adult Social Care Broker tool and provides [service API](http://playbook.hackney.gov.uk/API-Playbook/platform_api_vs_service_api#a-service-apis) capabilities for the [Brokerage Front End](https://github.com/LBHackney-IT/lbh-asc-brokerage-frontend/)


## Table of contents

  - [Getting started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Dockerised dependencies](#dockerised-dependencies)
    - [Installation](#installation)
  - [Usage](#usage)
    - [Running the application](#running-the-application)
    - [Databases](#databases)    
    - [Running the tests](#running-the-tests)
  - [Documentation](#documentation)
    - [API design](#api-design)
    - [Deployment](#deployment)
    - [Related repositories](#related-repositories)
  - [Active contributors](#active-contributors)
  - [License](#license)

## Getting started

### Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop)
- [.NET Core 3.1](https://dotnet.microsoft.com/download)

### Dockerised dependencies

- PostgreSQL 12

### Installation

Clone this repository

```sh
$ git clone git@github.com:LBHackney-IT/lbh-asc-brokerage-api.git
```

## Usage

### Running the application 

To serve the API locally with dotnet,
run `dotnet run` from within the [BrokerageApi](./BrokerageApi) project directory, i.e:

```sh
$ cd BrokerageApi && dotnet run
```

**The application will be served at http://localhost:5100**.


### Databases

Two local DBs will be available via Docker containers.

**The Development DB** is created and mantained via EF Core migrations. To create it from the last migration, run:

```sh
$ dotnet ef database update
```

The database will be served at port 5432


**The Test DB** is created and mantained based on EF Core migrations. To create/ update it, run any of the end to end tests.

The database will be served at port 5435


### Running the tests

There are two ways of running the tests: using the terminal and using an IDE.

#### Using the terminal

To run all tests, use:

```sh
$ make test
```

To run some tests i.e. single or a group, make sure your test db is up and running on Docker and then you can filter through tests, using the `--filter` argument of the
`dotnet test` command:

```sh
# E.g. for a specific test, use the test method name
$ dotnet test --filter CanAddAuditEvent
# E.g. for a file, use the test class name
$ dotnet test --filter AuditGatewayTests
```

See [Microsoft's documentation on running selective unit tests](https://docs.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest) for more information.

#### Using an IDE

Having your test db up and running on Docker will allow you to run the tests as normal in your IDE.

### API design

We use [SwaggerHub](https://swagger.io/tools/swaggerhub/) to document the API design:

- [Staging](https://wadss19f8f.execute-api.eu-west-2.amazonaws.com/staging/swagger/index.html) 

- [Production](https://3mhm6sj5o2.execute-api.eu-west-2.amazonaws.com/production/swagger/index.html) 

### Deployment

We have two environments:

- Staging (Mosaic-Staging AWS account)
- Production (Mosaic-Production AWS account)

and one deployment branch:

- `master` which deploys to Staging and Production

To deploy to Production, we first ensure that changes are verified in Staging and then we handle the deploy via CircleCI (see [CircleCI config](./.circleci/config.yml)).


### Related repositories

| Name | Purpose |
|-|-|
| [LBH Brokerage Frontend](https://github.com/LBHackney-IT/lbh-asc-brokerage-frontend/) | Provides the UI/UX for the Brokerage tool. |
| [API Playbook](http://playbook.hackney.gov.uk/API-Playbook/) | Provides guidance to the standards of APIs within Hackney. |

## Active contributors

- **Andrew White**, Tech Lead at Unboxed (andrew.white@hackney.gov.uk)
- **Kevin Sedgley**, Lead Developer at Unboxed (kevin.sedgley@hackney.gov.uk)
- **Joshua Dadswell**, Senior Software Engineer at IJYI (josh.dadswell@hackney.gov.uk)
- **Marta Pederiva**, Junior Developer at Hackney (marta.pederiva@hackney.gov.uk)

## License

[MIT License](LICENSE)