docker run -v ${pwd}/mongodbprepare.sh:/docker-entrypoint-initdb.d/mongodbprepare.sh -p 27017:27017 --name test-mongo-bc -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=secret -d mongo