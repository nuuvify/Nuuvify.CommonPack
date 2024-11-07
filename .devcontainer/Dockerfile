FROM lzocateli/dotnet-sdk:8.0.303-jammy-amd64

ARG PathApp=/workspaces
ARG TZ=America/Sao_Paulo
ARG DOTNET_ENVIRONMENT
ARG UserApp
ARG UserUID
ARG UserGID
ARG HttpProxyEnv
ARG HttpsProxyEnv
ARG NoProxyEnv
ARG NugetMode

ENV HTTP_PROXY=$HttpProxyEnv
ENV HTTPS_PROXY=$HttpProxyEnv
ENV NO_PROXY=$NoProxyEnv
ENV NUGET_XMLDOC_MODE=$NugetMode
ENV USER_UID=$UserUID
ENV USER_GID=$UserGID

RUN apt update -y && apt upgrade -y \
 && apt install -y \
 unzip zip fzf


RUN echo "Dotnet Environment: $DOTNET_ENVIRONMENT" \
    && echo "USER_GID: $USER_GID" \
    && echo "USER_UID: $USER_UID"

ENV DOTNET_ENVIRONMENT=$DOTNET_ENVIRONMENT
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone


RUN groupadd --gid $USER_GID $UserApp \
    && useradd --uid $USER_UID --gid $USER_GID -m $UserApp \
    && apt install -y sudo \
    && echo $UserApp ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$UserApp \
    && chmod 0440 /etc/sudoers.d/$UserApp


WORKDIR /home/$UserApp/$PathApp

USER ${UserApp}


# Adicionando comandos para instalar oh-my-posh e configurar zsh
RUN echo "Instalando oh-my-posh ..." \
    && mkdir -p /home/$UserApp/.config /home/$UserApp/.local/bin \
    # && cp $WORKFOLDER/.devcontainer/atomic.omp.json /home/$UserApp/.config/atomic.omp.json \
    && curl -s https://ohmyposh.dev/install.sh | bash -s -- -d /home/$UserApp/.local/bin \
    && echo 'source /home/$UserApp/.oh-my-zsh/custom/plugins/zsh-autosuggestions/zsh-autosuggestions.zsh' >> /home/$UserApp/.zshrc \
    && echo 'eval "$(oh-my-posh init zsh --config /home/$UserApp/.config/atomic.omp.json)"' >> /home/$UserApp/.zshrc \
    && chown -R $UserApp:$UserApp /home/$UserApp/.config /home/$UserApp/.zshrc

# Fonte do arquivo zshrc para aplicar as mudanças
RUN echo "source /home/$UserApp/.zshrc" >> /home/$UserApp/.bashrc



# Definindo o shell padrão para zsh
SHELL ["/bin/zsh", "-c"]

# Comando final para manter o container ativo
CMD ["zsh"]
