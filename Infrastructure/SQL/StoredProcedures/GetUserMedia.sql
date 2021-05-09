CREATE PROCEDURE SP_GetUserMedia @UserId as BIGINT
AS
    SET NOCOUNT ON;
SELECT [MediaId], [Title], [PosterPath]
FROM [UserMedia] AS UM
         INNER JOIN [Media] AS M ON UM.MediaId = M.Id
         INNER JOIN [User] AS U ON UM.UserId = U.Id
WHERE U.Id = @UserId
ORDER BY M.CreatedAt;