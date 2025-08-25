ALTER TABLE tesoreria.emisor ALTER COLUMN nit TYPE text;


                UPDATE tesoreria.emisor AS e
                SET nit = v.nit
                FROM (VALUES
                    ('2','860007738'),
                    ('65','900628110'),
                    ('7','890903938'),
                    ('19','860051705'),
                    ('9','860051135'),
                    ('12','860050750'),
                    ('13','860003020'),
                    ('6','860007660'),
                    ('23','890300279'),
                    ('32','860007335'),
                    ('51','860034313'),
                    ('40','800037800'),
                    ('52','860035827'),
                    ('58','900200960'),
                    ('53','900378212'),
                    ('61','900406150'),
                    ('63','860051894'),
                    ('62','900047981'),
                    ('57','890200756'),
                    ('47','900768933'),
                    ('121','900688066'),
                    ('66','890203088'),
                    ('1','860002964')
                ) AS v(codigo_homologado, nit)
                WHERE e.codigo_homologado::text = v.codigo_homologado;
            

INSERT INTO tesoreria."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250825161101_ChangeIssuerNitTypeToString', '9.0.3');


