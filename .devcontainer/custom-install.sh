#!/bin/bash

USERSHELL=$(getent passwd $USER | cut -d: -f7)
FILEZSH=$(which zsh)

if [[ "/usr/bin/zsh" != "$USERSHELL" ]]; then
    usermod -s $FILEZSH $USER
fi

FILEPOSH=$(which oh-my-posh)

if [ ! -f "$FILEPOSH" ]; then
    curl -s https://ohmyposh.dev/install.sh | bash -s
    mv ~/.zshrc ~/.zshrc-old
    cp $TESTE/.devcontainer/.zshrc ~/.zshrc
fi

