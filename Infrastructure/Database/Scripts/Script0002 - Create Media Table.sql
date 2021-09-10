CREATE TABLE media
(
    id           BIGSERIAL,
    imdb_id      VARCHAR(100) NOT NULL UNIQUE,
    title        VARCHAR(150) NOT NULL,
    plot         TEXT,
    poster_path  TEXT,
    release_date DATE,
    url          TEXT,
    year         SMALLINT,
    created_at   timestamp DEFAULT CURRENT_TIMESTAMP,
    updated_at   timestamp,
    CONSTRAINT PK_Media_Id PRIMARY KEY (id)
)