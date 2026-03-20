<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Category extends Model
{
    protected $fillable = [
        'name',
    ];

    public function components()
    {
        return $this->hasMany(Component::class, 'category_id', 'id');
    }
}
