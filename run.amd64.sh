docker run --rm -d -p 9000:9000 --name server vlkkarel/llmb-amd64
sleep 10
docker build --no-cache -t java-chatbot .
docker run --rm -it --link server java-chatbot