# 
#  ¦¦¦¦¦+  ¦¦¦¦¦¦+¦¦¦¦¦¦¦¦+¦¦¦¦¦¦¦+¦¦¦+   ¦¦+¦¦¦¦¦¦¦¦+¦¦+ ¦¦¦¦¦¦+ ¦¦¦+   ¦¦+
# ¦¦+--¦¦+¦¦+----++--¦¦+--+¦¦+----+¦¦¦¦+  ¦¦¦+--¦¦+--+¦¦¦¦¦+---¦¦+¦¦¦¦+  ¦¦¦
# ¦¦¦¦¦¦¦¦¦¦¦        ¦¦¦   ¦¦¦¦¦+  ¦¦+¦¦+ ¦¦¦   ¦¦¦   ¦¦¦¦¦¦   ¦¦¦¦¦+¦¦+ ¦¦¦
# ¦¦+--¦¦¦¦¦¦        ¦¦¦   ¦¦+--+  ¦¦¦+¦¦+¦¦¦   ¦¦¦   ¦¦¦¦¦¦   ¦¦¦¦¦¦+¦¦+¦¦¦
# ¦¦¦  ¦¦¦+¦¦¦¦¦¦+   ¦¦¦   ¦¦¦¦¦¦¦+¦¦¦ +¦¦¦¦¦   ¦¦¦   ¦¦¦+¦¦¦¦¦¦++¦¦¦ +¦¦¦¦¦
# +-+  +-+ +-----+   +-+   +------++-+  +---+   +-+   +-+ +-----+ +-+  +---+
#                                                                           
# If you modify this file from a Windows system, run the following command from powershell!!!
# (Get-Content -Path .\mongodbinitialization.sh -Raw) -replace "\r", "" | Set-Content -Path .\mongodbinitialization.sh -NoNewline -Force

#!/bin/bash

echo "Starting to create DB and collection..."

sleep 20

mongosh -u mongoadmin -p secret <<EOF
use testDBVoucherBlockChain
db.createCollection("dataBCVoucher")
db.createCollection("blockBCVoucher")
db.createCollection("dataBCVoucher-first")
db.createCollection("blockBCVoucher-first")
EOF

echo "Initialization completed."