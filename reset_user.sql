-- Resetar o usuário pedrohcd2007@gmail.com para Role = User
UPDATE Users 
SET Role = 'User' 
WHERE Email = 'pedrohcd2007@gmail.com';

-- Verificar o resultado
SELECT Id, Name, Email, Role, IsActive FROM Users WHERE Email = 'pedrohcd2007@gmail.com';
