#!/bin/bash


USERSHELL=$(getent passwd $USER | cut -d: -f7)
FILEZSH=$(which zsh)

if [[ "/usr/bin/zsh" != "$USERSHELL" ]]; then
    usermod -s $FILEZSH $USER
fi

FILEPOSH=$(which oh-my-posh)

if [ ! -f "$FILEPOSH" ]; then
    echo "Instalando oh-my-posh ..."
    mkdir -p ~/.config
    cp $WORKFOLDER/.devcontainer/atomic.omp.json ~/.config/atomic.omp.json
    curl -s https://ohmyposh.dev/install.sh | bash -s

    echo 'source ~/.oh-my-zsh/custom/plugins/zsh-autosuggestions/zsh-autosuggestions.zsh' >> ~/.zshrc
    echo 'eval "$(oh-my-posh init zsh --config ~/.config/atomic.omp.json)"' >> ~/.zshrc

    source ~/.zshrc
fi
