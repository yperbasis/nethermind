#!/bin/bash
set -e
BIN_PATH="bin/Debug/netcoreapp3.1/"
NETHERMIND_PATH="${HOME}/Work/nethermind/src/Nethermind/"
NETHERMIND_PLUGINS_PATH="${NETHERMIND_PATH}Nethermind.Runner/${BIN_PATH}plugins"
PLUGINS=plugins

rm -rf plugins
build_ndm_plugin.zsh
dotnet build
cp -r $NETHERMIND_PLUGINS_PATH $PLUGINS

docker-compose build
docker-compose up
