-- User "discord_id" index
CREATE INDEX IX_User_DiscordId ON "user"(discord_id);

-- Media "imdb_id" index
CREATE INDEX IX_Media_ImdbId ON media(imdb_id);