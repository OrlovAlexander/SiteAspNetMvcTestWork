USE [AppElmaTestWork]
GO

DECLARE	@return_value Int

EXEC	@return_value = [dbo].[sp_Document_Create]
		@Id = N'',
		@OriginalName = N'',
		@Description = N'',
		@Date = NULL,
		@Author = N'',
		@FileName = N''

SELECT	@return_value as 'Return Value'

GO
