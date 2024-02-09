# 3PROJ-BACK

## Description

This is the back-end of the 3PROJ project. It is a GraphQL Api working with Postgres.
The goal is to create a simple and efficient API to manage group expenses.

## Installation

clone the repository and run the following command inside the project folder:

```bash
docker compose up -d
```

This will start 2 services: the database (**db**) and the graphql api (**api**).

## Usage

The API is available at `http://localhost:3000/graphql`
The database is available at `http://localhost:5432`

## Configuration

The docker-compose file is configured to use the .env file at the root of the project. You can change the environment variables in this file to fit your needs.