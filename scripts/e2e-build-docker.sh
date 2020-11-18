#!/bin/bash
docker buildx build --platform=linux/amd64 -t nethermind . 

docker build -t nethermind-e2e -f Dockerfile.e2e .
