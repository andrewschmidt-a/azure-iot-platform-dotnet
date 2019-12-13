DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $DIR/config/WebService
azds up -d
cd $DIR/identity-gateway/WebService
azds up -d
cd $DIR/reverse-proxy
azds up -d
cd $DIR/iothub-manager/WebService
azds up -d
cd $DIR/device-telemetry/WebService
azds up -d
cd $DIR/storage-adapter/WebService
azds up -d
cd $DIR/tenant-manager/WebService
azds up -d
cd $DIR/asa-manager/WebService
azds up -d
cd $DIR/webui
azds up -d