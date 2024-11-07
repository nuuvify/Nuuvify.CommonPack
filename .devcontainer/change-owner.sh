#!/bin/bash

# Mudar o proprietário das pastas montadas para o usuário do container
sudo chown -R $HOST_USER:$HOST_USER /home/$HOST_USER/.nuget
sudo chown -R $HOST_USER:$HOST_USER /home/$HOST_USER/.ssh
sudo chown -R $HOST_USER:$HOST_USER /home/$HOST_USER/.microsoft
sudo chown -R $HOST_USER:$HOST_USER /home/$HOST_USER/.config
sudo chown -R $HOST_USER:$HOST_USER $APP_PATH

echo 'source /home/$HOST_USER/.oh-my-zsh/custom/plugins/zsh-autosuggestions/zsh-autosuggestions.zsh' >> /home/$HOST_USER/.zshrc
echo 'eval "$(oh-my-posh init zsh --config /home/$HOST_USER/.config/atomic.omp.json)"' >> /home/$HOST_USER/.zshrc
