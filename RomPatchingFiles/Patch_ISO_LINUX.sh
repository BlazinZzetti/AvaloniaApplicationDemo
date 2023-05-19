#!/bin/sh
cd "$(cd "$(dirname "$0")" && pwd)"
chmod +x $3
$3 -v -d -s $1 $4 $2
