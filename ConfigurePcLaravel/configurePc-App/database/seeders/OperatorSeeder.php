<?php

namespace Database\Seeders;

use App\Models\Operator;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class OperatorSeeder extends Seeder
{
    public function run(): void
    {
        Operator::create([
            'name'     => 'Operator',
            'email'    => 'operator@example.com',
            'password' => Hash::make('1234'),
        ]);
    }
}
