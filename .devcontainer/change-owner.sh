#!/bin/bash

# Mudar o proprietário das pastas montadas para o usuário do container
chown -R devcontainer:devcontainer /home/devcontainer/.nuget
chown -R devcontainer:devcontainer /home/devcontainer/.ssh
chown -R devcontainer:devcontainer /home/devcontainer/.microsoft
chown -R devcontainer:devcontainer /home/devcontainer/.config
