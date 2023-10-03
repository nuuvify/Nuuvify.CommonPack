#!/bin/bash

FILEZSH=$(which zsh)

if [ ! -f "$FILEZSH" ]; then
    curl -s https://ohmyposh.dev/install.sh | bash -s
    usermod -s $FILEZSH
fi
