#!/bin/bash

USERSHELL=$(getent passwd $USER | cut -d: -f7)
FILEZSH=$(which zsh)

if [[ "/usr/bin/zsh" != "$USERSHELL" ]]; then
    usermod -s $FILEZSH $USER
fi

FILEPOSH=$(which oh-my-posh)

if [ ! -f "$FILEPOSH" ]; then
    echo "Instalando oh-my-posh ..."
    curl -s https://ohmyposh.dev/install.sh | bash -s
    mv ~/.zshrc ~/.zshrc-old
    cp $WORKFOLDER/.devcontainer/atomic.omp.json ~/.config/atomic.omp.json
    cp $WORKFOLDER/.devcontainer/.zshrc ~/.zshrc
fi

