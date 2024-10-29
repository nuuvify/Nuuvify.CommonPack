FROM lzocateli/dotnet-sdk:8.0.303-jammy-amd64

ARG PathApp=/workspaces
ARG HttpProxyEnv=
ARG HttpsProxyEnv=
ARG NoProxyEnv=

ENV HTTP_PROXY=$HttpProxyEnv
ENV HTTPS_PROXY=$HttpProxyEnv
ENV NO_PROXY=$NoProxyEnv

RUN apt update -y && apt upgrade -y \
 && apt install -y unzip zip fzf

ARG TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone

RUN curl -s https://ohmyposh.dev/install.sh | bash -s
