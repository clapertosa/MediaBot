CREATE TABLE user_media
(
    user_id  BIGSERIAL NOT NULL,
    media_id BIGSERIAL NOT NULL,
    CONSTRAINT FK_UserMedia_UserId FOREIGN KEY (user_id) REFERENCES "user" (id),
    CONSTRAINT FK_UserMedia_MediaId FOREIGN KEY (media_id) REFERENCES media (id)
)