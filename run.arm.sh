docker run --rm -d -p 9000:9000 --name server vlkkarel/llmb
sleep 10
docker build --no-cache -t chatbot .
docker run --rm -it --link server chatbot
