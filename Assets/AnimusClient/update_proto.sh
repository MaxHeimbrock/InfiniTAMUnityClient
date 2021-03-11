#!/bin/bash
#
# Copyright 2019 Daniel Camilleri
set -e 

git clone https://github.com/dcam0050/AnimusMessages.git
rm -rf Common/Proto
mkdir Common/Proto
cp AnimusMessages/csharp/* Common/Proto
rm -rf AnimusMessages