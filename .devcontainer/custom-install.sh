#!/bin/bash

FILEZSH=$(which zsh)

if [ ! -f "$FILEZSH" ]; then
    curl -s https://ohmyposh.dev/install.sh | bash -s
    mv ~/.zshrc ~/.zshrc-old
    cp $TESTE/.devcontainer/.zshrc ~/.zshrc
fi

USERSHELL=$(getent passwd $USER | cut -d: -f7)

if [[ "/bin/zsh" != "$USERSHELL" ]]; then
    usermod -s $FILEZSH $USER
fi
