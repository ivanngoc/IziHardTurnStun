Сборка проекта: скомпилировать; разархивировать Proxy.zip.
Запуск: запускаем с параметрами IP turn сервера, порт turn сервера, IP клиента с браузером.

Установка coturn: 
    1. Установить Coturn через apt-get. 
    2. По пути /etc/default/coturn раскоментировать строку TURNSERVER_ENABLED=1
    3. По пути /etc/coturn/turnserver.conf сохранить следующие настройки
```
                    listening-port=3478
                    tls-listening-port=3478
                    
                    listening-ip=your internal IP
                    external-ip=your external IP/your internal IP
                    
                    min-port=49152
                    max-port=65535
                    verbose
                    user=testuser:password
                    
                    realm=myrealm
                    cli-password=test123
                    allow-loopback-peers
```
После этого выполнить команду sudo systemctl restart coturn

Пока аутентификация работает только для user=testuser:password. Возможно добавить указание этих параметров через параметры при запуске.
Также пока проходит только http запросы (https запросы не проходят). Также большие страницы, картинки, файлы и т.п. загружаются долго. Необходимо оптимизировать.

    
