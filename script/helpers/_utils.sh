error() {
    echo "$@" >&2
}

fatal() {
    error "$@"
    exit 1
}

set_source_and_root_dir() {
    { set +x; } 2>/dev/null
    source_dir="$( cd -P "$( dirname "$0" )" >/dev/null 2>&1 && pwd )"
    root_dir=$(cd "$source_dir" && cd ../ && pwd)
    cd "$root_dir"
}

install_if_missing() {
    if ! type "$1" >/dev/null 2>&1; then
        echo "Installing $1"
        if [ -n "$2" ]; then
            echo "Reason: $2"
        else
            echo "Reason: $1 is required to run the project."
        fi
        brew install "$1"
    else
        echo "✅$1 installed."
    fi
}
