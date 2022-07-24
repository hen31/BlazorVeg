DECLARE @intFlag INT
SET @intFlag = 1
WHILE (@intFlag <=20)
BEGIN
    INSERT INTO ProductReviews VALUES(NEWID(), '5ABB4A48-D042-4CBC-561A-08D84937E912', 'Wat vind ik hiervan, tja wat zal ik zeggen',  (CAST((RAND() * 1000000) AS INT )% 5) +1, 'DBEE278C-AEDD-4BAB-481B-08D84B9E99C0', '2020-09-15 21:38:06.6022996','2020-09-15 21:38:06.6022996');
    SET @intFlag = @intFlag + 1
END
GO


SELECT * FROM ProductReviews

