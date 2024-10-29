#!/bin/bash
chmod 777 -R .devcontainer/
chmod +x .devcontainer/*.sh

echo 'podman() {
    if [ "$1" == "buildx" ] && [ "$2" == "version" ]; then
        command podman version
    else
        command podman "$@"
    fi
}' >> ~/.bashrc

source ~/.bashrc

