#!/bin/bash

# Mudar o proprietário das pastas montadas para o usuário do container
# sudo chown -R $HOST_USER:$HOST_USER /root/.nuget
# sudo chown -R $HOST_USER:$HOST_USER /root/.ssh
# sudo chown -R $HOST_USER:$HOST_USER /root/.microsoft
# sudo chown -R $HOST_USER:$HOST_USER /root/.config
# sudo chown -R $HOST_USER:$HOST_USER $APP_PATH

mkdir -p /root/.config
cp $APP_PATH/.devcontainer/atomic.omp.json /root/.config/atomic.omp.json

echo 'source /root/.oh-my-zsh/custom/plugins/zsh-autosuggestions/zsh-autosuggestions.zsh' >> /root/.zshrc
echo 'eval "$(oh-my-posh init zsh --config /root/.config/atomic.omp.json)"' >> /root/.zshrc
