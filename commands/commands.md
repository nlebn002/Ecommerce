 # docker compose
 docker compose up --build        
 docker compose down -v    

 # docker compose ephemeral
 docker compose -f docker-compose-ephemeral.yml up -d --build
 docker compose -f docker-compose-ephemeral.yml down -v