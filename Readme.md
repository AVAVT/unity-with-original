# Wipe DB:

./postchain-node/postchain.sh wipe-db -nc config/node-config.properties

# Compile Code

./postchain-node/multigen.sh config/run.xml -d src -o target/

# Start Node

./postchain-node/postchain.sh run-node-auto -d target
