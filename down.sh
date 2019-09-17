DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $DIR/config/WebService
azds down -y
cd $DIR/identity-gateway/WebService
azds down -y
cd $DIR/reverse-proxy
azds down -y
cd $DIR/iothub-manager/WebService
azds down -y
cd $DIR/device-telemetry/WebService
azds down -y
cd $DIR/storage-adapter/WebService
azds down -y
cd $DIR/tenant-manager/WebService
azds down -y
cd $DIR/webui
azds down -y