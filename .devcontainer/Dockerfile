ARG BUILDPLATFORM=linux/amd64

FROM --platform=$BUILDPLATFORM lzocateli/dotnet-sdk:8.0.303-jammy-amd64


ENV HTTP_PROXY=
ENV HTTPS_PROXY=
ENV NO_PROXY=

# ENV http_proxy=http://host.docker.internal:3128
# ENV https_proxy=http://host.docker.internal:3128


RUN apt update -y \
    && apt upgrade -y

ARG TZ=America/Sao_Paulo
ARG DOTNET_ENVIRONMENT

RUN echo "Dotnet Environment: $DOTNET_ENVIRONMENT"
ENV DOTNET_ENVIRONMENT=$DOTNET_ENVIRONMENT
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone

ENV LD_LIBRARY_PATH=/root/app/clidriver/lib:/root/app/clidriver/lib/libdb2.so \
    PATH=$PATH:/root/app/clidriver/bin:/root/app/clidriver/lib:/usr/local/lib
