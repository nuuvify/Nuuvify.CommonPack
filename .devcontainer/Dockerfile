# Veja aqui caso precise gerar outra imagem localmente: 
# https://github.com/lzocateli/containers/blob/main/dotnet-sdk-devcontainer/readme.md

ARG BUILDPLATFORM=linux/amd64

FROM --platform=$BUILDPLATFORM lzocateli/dotnet-sdk:8.0.303-jammy-amd64

ENV HTTP_PROXY=http://proxy.cat.com:80
ENV HTTPS_PROXY=http://proxy.cat.com:80
ENV NO_PROXY=cat.com

ARG PathApp=/workspaces

RUN apt update -y \
 && apt upgrade -y

ARG UserApp=root
ARG TZ=America/Sao_Paulo
ARG DOTNET_ENVIRONMENT

RUN echo "Dotnet Environment: $DOTNET_ENVIRONMENT"
ENV DOTNET_ENVIRONMENT=$DOTNET_ENVIRONMENT
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone

ENV LD_LIBRARY_PATH=/$PathApp/clidriver/lib:/$PathApp/clidriver/lib/libdb2.so \
    PATH=$PATH:/$PathApp/clidriver/bin:/$PathApp/clidriver/lib
