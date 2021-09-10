CREATE TABLE "user"
(
    id            BIGSERIAL,
    discord_id    BIGSERIAL   NOT NULL UNIQUE,
    username      VARCHAR(35) NOT NULL,
    discriminator CHAR(4)     NOT NULL,
    is_bot        BOOLEAN,
    public_flags  SMALLINT,
    created_at    timestamp DEFAULT CURRENT_TIMESTAMP,
    updated_at    timestamp,
    CONSTRAINT PK_User_Id PRIMARY KEY (id)
)