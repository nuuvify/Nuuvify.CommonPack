FROM lzocateli/dotnet-sdk:8.0.303-jammy-amd64

ARG PathApp=/workspaces

RUN apt update -y && apt upgrade -y

ARG TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone

