#!/bin/bash

if [ ! -f "~/.config/atomic.omp.json" ]; then
    echo "Configurando oh-my-posh ..."
    mv ~/.zshrc ~/.zshrc-old
    mkdir -p ~/.config && cp $WORKFOLDER/.devcontainer/atomic.omp.json ~/.config/atomic.omp.json
    cp $WORKFOLDER/.devcontainer/.zshrc ~/.zshrc
    ## Inicio Somente para o Ansible
    chmod -R 775 $WORKFOLDER
    ## final Somente para o Ansible
fi

