-- Script para popular subcategorias de exemplo
-- Execute este script após rodar as migrations

-- Subcategorias para Mouses (assumindo que Mouses tem ID 1)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('Razer', 'Produtos da marca Razer', 1, 1, 1),
('Logitech', 'Produtos da marca Logitech', 1, 1, 2),
('Redragon', 'Produtos da marca Redragon', 1, 1, 3),
('Corsair', 'Produtos da marca Corsair', 1, 1, 4),
('SteelSeries', 'Produtos da marca SteelSeries', 1, 1, 5);

-- Subcategorias para Teclados (assumindo que Teclados tem ID 2)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('Razer', 'Teclados Razer', 2, 1, 1),
('Logitech', 'Teclados Logitech', 2, 1, 2),
('Redragon', 'Teclados Redragon', 2, 1, 3),
('Corsair', 'Teclados Corsair', 2, 1, 4),
('HyperX', 'Teclados HyperX', 2, 1, 5);

-- Subcategorias para Headsets (assumindo que Headsets tem ID 3)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('Razer', 'Headsets Razer', 3, 1, 1),
('Logitech', 'Headsets Logitech', 3, 1, 2),
('HyperX', 'Headsets HyperX', 3, 1, 3),
('SteelSeries', 'Headsets SteelSeries', 3, 1, 4),
('Corsair', 'Headsets Corsair', 3, 1, 5);

-- Subcategorias para Monitores (assumindo que Monitores tem ID 4)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('ASUS', 'Monitores ASUS', 4, 1, 1),
('LG', 'Monitores LG', 4, 1, 2),
('Samsung', 'Monitores Samsung', 4, 1, 3),
('AOC', 'Monitores AOC', 4, 1, 4),
('Dell', 'Monitores Dell', 4, 1, 5);

-- Subcategorias para Processadores (assumindo que Processadores tem ID 5)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('Intel', 'Processadores Intel', 5, 1, 1),
('AMD', 'Processadores AMD', 5, 1, 2);

-- Subcategorias para Placas de Vídeo (assumindo que Placas de Vídeo tem ID 6)
INSERT INTO Subcategories (Name, Description, CategoryId, IsActive, DisplayOrder) VALUES
('NVIDIA', 'Placas de vídeo NVIDIA', 6, 1, 1),
('AMD', 'Placas de vídeo AMD', 6, 1, 2),
('ASUS', 'Placas de vídeo ASUS', 6, 1, 3),
('MSI', 'Placas de vídeo MSI', 6, 1, 4),
('Gigabyte', 'Placas de vídeo Gigabyte', 6, 1, 5);
