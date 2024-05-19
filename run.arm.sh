docker run --rm -d -p 9000:9000 --name server vlkkarel/llmb
echo "LLM Server is running"
sleep 10
docker build --no-cache -t chatbot .
docker run --rm -it --link server chatbot
