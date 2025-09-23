#!/bin/bash

if [ ! -f "~/.config/atomic.omp.json" ]; then
    echo "Configurando oh-my-posh ..."
    mv ~/.zshrc ~/.zshrc-old
    mkdir -p ~/.config && cp $WORKFOLDER/.devcontainer/atomic.omp.json ~/.config/atomic.omp.json
    cp $WORKFOLDER/.devcontainer/.zshrc ~/.zshrc
    # az config set extension.dynamic_install_allow_preview=false
    chmod -R 775 $WORKFOLDER
fi

