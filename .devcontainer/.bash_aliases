# enable color support of ls and also add handy aliases
if [ -x /usr/bin/dircolors ]; then
    test -r ~/.dircolors && eval "$(dircolors -b ~/.dircolors)" || eval "$(dircolors -b)"
    alias ls='ls --color=auto'
    alias grep='grep --color=auto'
    alias fgrep='fgrep --color=auto'
    alias egrep='egrep --color=auto'
fi

# some more ls aliases
alias ll='ls -lha'
alias la='ls -A'
alias l='ls -CF'

alias gh='podman run --rm -it -v "$(pwd):/workspace" -w /workspace -v ~/.config/gh:/root/.config/gh ghcr.io/supportpal/github-gh-cli:2.31.0 gh "$@"'
alias terraform='podman run --rm -it -v "$(pwd):/workspace" -w /workspace hashicorp/terraform:1.3.7 "$@"'
alias az='podman run --rm -it -v ~/.ssh:/root/.ssh -v ~/.azure:/root/.azure -v ~/.azure-devops:/root/.azure-devops -v "$(pwd):/workspace" -w /workspace mcr.microsoft.com/azure-cli:2.49.0 az "$@"'
alias ng='podman run --rm -it -v "$(pwd):/workspace" -w /workspace -v /tmp/ng:/tmp/ng alexsuch/angular-cli:12.2.18 ng "$@"'
alias node='podman run --rm -it -v "$(pwd):/workspace" -w /workspace node:16.17.1-alpine3.16 "$@"'
alias npm='podman run --rm -it -v "$(pwd):/workspace" -w /workspace -v ~/.npmrc:/root/.npmrc -v ~/npm-cache:/root/.npm -v /tmp/npm:/tmp/npm node:16.17.1-alpine3.16 npm "$@"'

# Add an "alert" alias for long running commands.  Use like so:
#   sleep 10; alert
alias alert='notify-send --urgency=low -i "$([ $? = 0 ] && echo terminal || echo error)" "$(history|tail -n1|sed -e '\''s/^\s*[0-9]\+\s*//;s/[;&|]\s*alert$//'\'')"'
