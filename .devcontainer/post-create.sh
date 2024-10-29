#!/bin/bash

FILEPOSH=$(which oh-my-posh)

if [ ! -e "$HOME/.zshrc-old" ]; then
    mv $HOME/.zshrc $HOME/.zshrc-old
    mkdir -p $HOME/.config

    cp $WORKFOLDER/.devcontainer/atomic.omp.json $HOME/.config/atomic.omp.json
    cp $WORKFOLDER/.devcontainer/.zshrc $HOME/.zshrc
else
    echo "oh-my-posh ja configurado"
fi
