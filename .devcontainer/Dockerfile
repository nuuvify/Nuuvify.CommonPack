FROM lzocateli/dotnet-sdk-3.1.426-focal-amd64:1.0.0

ARG PathApp=/workspaces

RUN apt update -y && apt upgrade -y

# RUN SNIPPET="export PROMPT_COMMAND='history -a' && export HISTFILE=/commandhistory/.bash_history" \
#     && mkdir /commandhistory \
#     && touch /commandhistory/.bash_history \
#     && chown -R $USERNAME /commandhistory \
#     && echo "$SNIPPET" >> "/home/$USERNAME/.bashrc"

# ARG UserApp=nonroot

# RUN useradd -ms /bin/bash $UserApp 

ENV TZ=Brazil/East
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ >/etc/timezone

# WORKDIR $PathApp

# USER $UserApp
