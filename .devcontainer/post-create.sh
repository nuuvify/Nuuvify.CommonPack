#!/bin/bash

FILEPOSH=$(which oh-my-posh)

if [ ! -f "$FILEPOSH" ]; then
    echo "Instalando oh-my-posh ... em: ${HOME}"

    mv "${HOME}/.zshrc ${HOME}/.zshrc-old"
    mkdir -p "${HOME}/.config"

    sudo apt-get update

    # Instalar pacotes necessários
    sudo apt-get install -y fzf

    # Baixar e instalar o Oh-My-Posh
    curl -s https://ohmyposh.dev/install.sh | bash -s

    # Copiar temas para o diretório do usuário
    mkdir -p "${HOME}/.cache/oh-my-posh"
    cp -r /root/.cache/oh-my-posh/* "${HOME}/.cache/oh-my-posh"

    # Inicializar o Oh-My-Posh com PowerShell
    # oh-my-posh --init --shell pwsh --config ~/jandedobbeleer.omp.json | Invoke-Expression

    cp $WORKFOLDER/.devcontainer/atomic.omp.json "${HOME}/.config/atomic.omp.json"
    cp $WORKFOLDER/.devcontainer/.zshrc "${HOME}/.zshrc"
fi

source "${HOME}/.zshrc"