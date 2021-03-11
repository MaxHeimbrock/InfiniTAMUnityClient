#!/usr/bin/env bash

set -e
IPADDR=http://192.168.1.117:8111

pushd ../AnimusRobot
ZIPNAME=csharp_bindings.zip
wget --http-user=$MYUSER --http-password=$MYPASS $IPADDR/repository/download/Animus_AnimusRobot_linuxamd64/.lastSuccessful/$ZIPNAME

unzip -q $ZIPNAME -d NewSDK
rm $ZIPNAME

rm -rf AnimusRobotSDK
rm -f NewSDK/*.so
rm -f NewSDK/*.h
mv NewSDK AnimusRobotSDK
rm -rf NewSDK
cp ../AnimusCommon/format_namespace.sh AnimusRobotSDK
cd AnimusRobotSDK
./format_namespace.sh AnimusRobot
rm format_namespace.sh

popd

#-------------------------------------------------------------------
pushd ../AnimusClient
ZIPNAME=csharp_bindings_paid.zip
wget --http-user=$MYUSER --http-password=$MYPASS $IPADDR/repository/download/Animus_AnimusClient_linuxamd64/.lastSuccessful/$ZIPNAME

unzip -q $ZIPNAME -d NewSDK
rm $ZIPNAME

rm -rf AnimusClientSDK
rm -f NewSDK/*.so
rm -f NewSDK/*.h
mv NewSDK AnimusClientSDK
rm -rf NewSDK
cp ../AnimusCommon/format_namespace.sh AnimusClientSDK
cd AnimusClientSDK
./format_namespace.sh AnimusClient
rm format_namespace.sh

popd
