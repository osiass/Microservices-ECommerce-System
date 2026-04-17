#!/bin/bash
echo "--- IDENTITY DB USERS ---"
psql -U admin -d IdentityDb -c "SELECT COUNT(*) FROM \"AppUsers\";"
psql -U admin -d IdentityDb -c "SELECT \"UserName\", \"Role\" FROM \"AppUsers\" LIMIT 5;"

echo "--- CATALOG DB PRODUCTS ---"
psql -U admin -d CatalogDb -c "SELECT COUNT(*) FROM \"Products\";"
psql -U admin -d CatalogDb -c "SELECT \"Name\", \"ImageUrl\" FROM \"Products\" LIMIT 3;"
